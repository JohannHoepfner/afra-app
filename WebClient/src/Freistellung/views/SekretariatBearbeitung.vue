<script setup>
import { computed, ref } from 'vue';
import { Button, useToast } from 'primevue';
import { mande } from 'mande';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { useFreistellungStore } from '@/Freistellung/stores/freistellung.js';
import FreistellungsantragCard from '@/Freistellung/components/FreistellungsantragCard.vue';
import { statusSeverity, statusLabel } from '@/Freistellung/helpers/formatters.js';

const toast = useToast();
const store = useFreistellungStore();

const navItems = [
    { label: 'Freistellungsantrag', route: { name: 'Freistellung-Sekretariat' } },
];

const confirming = ref(null);

await store.updateSekretariatAntraege();

// Split client-side: pending confirmation = AlleLehrerGenehmigt, processed = everything else
const pendingAntraege = computed(
    () => store.sekretariatAntraege?.filter((a) => a.status === 'AlleLehrerGenehmigt') ?? [],
);
const processedAntraege = computed(
    () => store.sekretariatAntraege?.filter((a) => a.status !== 'AlleLehrerGenehmigt') ?? [],
);

async function bestaetigen(antragId) {
    confirming.value = antragId;
    const api = mande(`/api/freistellung/sekretariat/${antragId}/bestaetigen`);
    try {
        await api.put({});
        toast.add({
            severity: 'success',
            summary: 'Bestätigt',
            detail: 'Der Freistellungsantrag wurde vom Sekretariat bestätigt und wartet auf die Genehmigung des Schulleiters.',
            life: 3000,
        });
        store.sekretariatAntraege = null;
        await store.updateSekretariatAntraege();
    } catch (e) {
        const errorMessage = e.body?.error ?? 'Ein unbekannter Fehler ist aufgetreten.';
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: errorMessage,
            life: 5000,
        });
    } finally {
        confirming.value = null;
    }
}
</script>

<template>
    <NavBreadcrumb :items="navItems" />

    <h1>Freistellungsanträge (Sekretariat)</h1>

    <!-- Pending confirmation section -->
    <h2 class="text-lg font-semibold mt-4 mb-1">Warten auf Bestätigung</h2>
    <p class="mb-3 text-sm">
        Die folgenden Anträge wurden von allen betroffenen Lehrkräften und Mentor:innen
        genehmigt und warten auf die Bestätigung durch das Sekretariat.
    </p>

    <p v-if="!pendingAntraege.length" class="mt-2">
        Aktuell liegen keine zu bearbeitenden Freistellungsanträge vor.
    </p>

    <div v-else class="flex flex-col gap-4">
        <FreistellungsantragCard
            v-for="antrag in pendingAntraege"
            :key="antrag.id"
            :antrag="antrag"
            :showStudent="true"
            dateTagSeverity="warn"
        >
            <Button
                label="Antrag bestätigen"
                icon="pi pi-check-circle"
                severity="success"
                size="small"
                :loading="confirming === antrag.id"
                @click="bestaetigen(antrag.id)"
            />
        </FreistellungsantragCard>
    </div>

    <!-- Already-processed section -->
    <h2 class="text-lg font-semibold mt-8 mb-2">Bereits bearbeitete Anträge</h2>

    <p v-if="!processedAntraege.length" class="mt-2">
        Es wurden noch keine Freistellungsanträge abschließend bearbeitet.
    </p>

    <div v-else class="flex flex-col gap-4">
        <FreistellungsantragCard
            v-for="antrag in processedAntraege"
            :key="antrag.id"
            :antrag="antrag"
            :showStudent="true"
            :showStunden="false"
            :showEntscheidungen="false"
            :muted="true"
            dateTagSeverity="secondary"
            :statusTag="{
                severity: statusSeverity[antrag.status],
                value: statusLabel[antrag.status],
            }"
        />
    </div>
</template>

<style scoped></style>
