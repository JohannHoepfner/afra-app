<script setup>
import { Button, Column, DataTable, useToast } from 'primevue';
import { ref } from 'vue';
import { mande } from 'mande';
import { useUser } from '@/stores/user';
import StudentOverview from '@/Otium/components/Overview/StudentOverview.vue';
import { formatDayOfWeekFromEnum } from '@/helpers/formatters';

const loading = ref(true);
const user = useUser();
const toast = useToast();
const termine = ref(null);
const all = ref(false);
const profunda = ref(null);

async function fetchProfunda() {
    const api = mande('/api/profundum/sus/dashboard');
    try {
        profunda.value = await api.get();
    } catch (e) {
        console.error('Could not load profunda enrollments', e);
    }
}

async function fetchData(getAll = false) {
    loading.value = true;
    const dataGetter = mande('/api/otium/student');
    try {
        termine.value = await (getAll ? dataGetter.get('all') : dataGetter.get());
        all.value = getAll;
    } catch (e) {
        await user.update();
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Es ist ein Fehler beim Laden der Daten aufgetreten.',
        });
        console.error(e);
    } finally {
        loading.value = false;
    }
}

fetchData();
fetchProfunda();
</script>

<template>
    <h1>Hallo {{ user.user.vorname }}</h1>
    <!-- TODO: Introduce view for students that are tutors of otia. -->
    <template v-if="profunda && profunda.length > 0">
        <h2>Deine Profunda</h2>
        <DataTable :value="profunda" size="small">
            <Column header="Quartal">
                <template #body="{ data }">
                    {{ data.jahr }}/{{ data.jahr + 1 }} {{ data.quartal }}
                </template>
            </Column>
            <Column header="Wochentag">
                <template #body="{ data }">
                    {{ formatDayOfWeekFromEnum(data.wochentag) }}
                </template>
            </Column>
            <Column field="bezeichnung" header="Profundum" />
            <Column field="ort" header="Ort" />
        </DataTable>
    </template>
    <h2 v-if="!all">Deine nächsten Veranstaltungen</h2>
    <p v-if="!all">Gezeigt werden die Veranstaltungen der nächsten drei Wochen.</p>
    <h2 v-if="all">Alle Veranstaltungen</h2>
    <StudentOverview :termine="termine" show-katalog />
    <Button
        v-if="!all"
        class="mt-4"
        @click="fetchData(true)"
        label="Alle anzeigen"
        severity="secondary"
        :loading="loading"
    />
</template>

<style scoped></style>
