<script setup>
import { Button } from 'primevue';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { useFreistellungStore } from '@/Freistellung/stores/freistellung.js';
import FreistellungsantragCard from '@/Freistellung/components/FreistellungsantragCard.vue';
import { statusSeverity, statusLabel } from '@/Freistellung/helpers/formatters.js';

const store = useFreistellungStore();

const navItems = [{ label: 'Freistellungsantrag', route: { name: 'Freistellung-Meine' } }];

await store.updateMeineAntraege();
</script>

<template>
    <NavBreadcrumb :items="navItems" />

    <div class="flex items-center justify-between">
        <h1>Meine Freistellungsanträge</h1>
        <Button
            label="Neuer Antrag"
            icon="pi pi-plus"
            :to="{ name: 'Freistellung-Neu' }"
            as="router-link"
            size="small"
        />
    </div>

    <p v-if="!store.meineAntraege?.length" class="mt-4">
        Du hast noch keine Freistellungsanträge gestellt.
    </p>

    <div v-else class="flex flex-col gap-4 mt-4">
        <FreistellungsantragCard
            v-for="antrag in store.meineAntraege"
            :key="antrag.id"
            :antrag="antrag"
            :statusTag="{
                severity: statusSeverity[antrag.status],
                value: statusLabel[antrag.status],
            }"
        />
    </div>
</template>

<style scoped></style>
