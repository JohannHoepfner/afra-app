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
    { label: 'Freistellungsantrag (Schulleiter)', route: { name: 'Freistellung-Schulleiter' } },
];

const approving = ref(null);

await store.updateSchulleiterAntraege();

// Split: pending = Bestaetigt (Sekretariat confirmed, Schulleiter hasn't acted yet)
const pendingAntraege = computed(
    () => store.schulleiterAntraege?.filter((a) => a.status === 'Bestaetigt') ?? [],
);
const processedAntraege = computed(
    () => store.schulleiterAntraege?.filter((a) => a.status !== 'Bestaetigt') ?? [],
);

async function bestaetigen(antragId) {
    approving.value = antragId;
    const api = mande(`/api/freistellung/schulleiter/${antragId}/bestaetigen`);
    try {
        await api.put({});
        toast.add({
            severity: 'success',
            summary: 'Genehmigt',
            detail: 'Der Freistellungsantrag wurde endgültig genehmigt.',
            life: 3000,
        });
        store.schulleiterAntraege = null;
        await store.updateSchulleiterAntraege();
    } catch (e) {
        const errorMessage = e.body?.error ?? 'Ein unbekannter Fehler ist aufgetreten.';
        toast.add({
            severity: 'error',
            summary: 'Fehler',
            detail: errorMessage,
            life: 5000,
        });
    } finally {
        approving.value = null;
    }
}
</script>

<template>
    <NavBreadcrumb :items="navItems" />

    <h1>Freistellungsanträge (Schulleiter)</h1>

    <!-- Pending section -->
    <h2 class="text-lg font-semibold mt-4 mb-1">Warten auf abschließende Genehmigung</h2>
    <p class="mb-3 text-sm">
        Die folgenden Anträge wurden vom Sekretariat bestätigt und warten auf Ihre abschließende
        Genehmigung.
    </p>

    <p v-if="!pendingAntraege.length" class="mt-2">
        Aktuell liegen keine Freistellungsanträge zur Genehmigung vor.
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
                label="Antrag genehmigen"
                icon="pi pi-check-circle"
                severity="success"
                size="small"
                :loading="approving === antrag.id"
                @click="bestaetigen(antrag.id)"
            />
        </FreistellungsantragCard>
    </div>

    <!-- Already-processed section -->
    <h2 class="text-lg font-semibold mt-8 mb-2">Bereits bearbeitete Anträge</h2>

    <p v-if="!processedAntraege.length" class="mt-2">
        Es wurden noch keine Freistellungsanträge endgültig bearbeitet.
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
