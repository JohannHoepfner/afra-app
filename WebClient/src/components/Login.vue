<script setup>
import { Form } from '@primevue/forms';
import { Button, Checkbox, FloatLabel, InputText, Password, useToast } from 'primevue';
import { onMounted, ref } from 'vue';
import { mande } from 'mande';
import { useUser } from '@/stores/user';

const loading = ref(false);
const oidcEnabled = ref(null); // null = loading, true = oidc, false = local form
const user = useUser();
const toast = useToast();

onMounted(async () => {
    try {
        const config = await mande('/api/auth/config').get();
        oidcEnabled.value = config.oidcEnabled === true;
    } catch {
        oidcEnabled.value = false;
    }
});

const loginWithKeycloak = () => {
    window.location.href = `/api/user/login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
};

const submit = async (evt) => {
    if (loading.value) return;
    const username = evt.states['username'].value;
    const password = evt.states['password'].value;
    const remember = !!evt.states['remember'].value;
    if (!(username && password)) return;
    loading.value = true;
    try {
        await mande('/api/user/login').post(
            {
                username: username,
                password: password,
                rememberMe: remember,
            },
            { responseAs: 'response' },
        );
        await user.update();
    } catch (error) {
        if (error.response.status === 401) {
            toast.add({
                severity: 'error',
                summary: 'Fehler',
                detail: 'Fehlerhafte Anmeldedaten',
                life: 5000,
            });
        } else if (error.response.status === 429) {
            toast.add({
                severity: 'error',
                summary: 'Zu viele Anmeldeversuche',
                detail: 'Bitte warten Sie 5 Minuten, bevor Sie es erneut versuchen.',
                life: 5000,
            });
        } else {
            toast.add({
                severity: 'error',
                summary: 'Fehler',
                detail: 'Ein unbekannter Fehler ist aufgetreten',
                life: 5000,
            });
        }
    } finally {
        loading.value = false;
    }
};
</script>

<template>
    <!-- OIDC login -->
    <div v-if="oidcEnabled === true" class="flex flex-col gap-6 mt-8">
        <Button
            fluid
            label="Mit Keycloak anmelden"
            icon="pi pi-sign-in"
            severity="secondary"
            @click="loginWithKeycloak"
        />
    </div>

    <!-- Local login form (OIDC disabled or config still loading) -->
    <Form v-else-if="oidcEnabled === false" @submit="submit" class="flex flex-col gap-6 mt-8">
        <FloatLabel variant="on">
            <InputText id="username" fluid name="username" type="text" />
            <label for="username">Nutzername</label>
        </FloatLabel>
        <FloatLabel variant="on">
            <Password name="password" :feedback="false" fluid toggle-mask input-id="password" />
            <label for="password">Passwort</label>
        </FloatLabel>
        <div class="flex items-center gap-2">
            <Checkbox name="remember" input-id="remember" binary />
            <label for="remember" class="cursor-pointer font-medium text-surface-500"
                >Angemeldet bleiben</label
            >
        </div>
        <Button :loading="loading" fluid label="Einloggen" severity="secondary" type="submit" />
    </Form>
</template>

<style scoped></style>
