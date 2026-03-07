<script setup>
import { useUser } from '@/stores/user';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import AuslastungsStatistik from '@/Otium/components/Management/AuslastungsStatistik.vue';

const user = useUser();

const navItems = [
    { label: 'Otium', route: { name: 'Katalog' } },
    { label: 'Verwaltung', route: { name: 'Verwaltung' } },
    { label: 'Auslastungsstatistik' },
];
</script>

<template>
    <template v-if="user.isOtiumsverantwortlich">
        <NavBreadcrumb :items="navItems" />
        <h1>Auslastungsstatistik</h1>
        <AuslastungsStatistik />
    </template>
    <template v-else>
        <h1>Sie sind nicht autorisiert, diese Seite zu nutzen.</h1>
    </template>
</template>

<style scoped></style>
