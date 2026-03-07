<script setup>
import { ref } from 'vue';
import { mande } from 'mande';
import { useToast } from 'primevue';
import { useUser } from '@/stores/user';
import AfraAuslastungsChart from '@/Otium/components/Management/AfraAuslastungsChart.vue';

const user = useUser();
const toast = useToast();
const loading = ref(true);
const series = ref([]);

async function setup() {
    try {
        const api = mande('/api/otium/management/statistics');
        series.value = await api.get();
    } catch {
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: 'Die Statistikdaten konnten nicht geladen werden.',
        });
        await user.update();
    } finally {
        loading.value = false;
    }
}

setup();
</script>

<template>
    <p class="mb-3 text-surface-500 text-sm">
        Anwesenheiten relativ zur Kapazität aller Otia über die Zeit. Jede Linie entspricht einem
        Otium; die Y-Achse zeigt die prozentualen Anwesenheiten bezogen auf die Kapazität. Otia
        können über die Legende ein- und ausgeblendet werden.
    </p>
    <template v-if="!loading">
        <AfraAuslastungsChart :series="series" />
    </template>
    <template v-else>
        <div class="h-[420px] flex items-center justify-center text-surface-400">Lade…</div>
    </template>
</template>

<style scoped></style>
