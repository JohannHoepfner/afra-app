<script setup>
import Plotly from 'plotly.js-dist-min';
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';

const props = defineProps({
    /** Array of { otiumId, bezeichnung, dataPoints: [{ datum, auslastung }] } */
    series: {
        type: Array,
        default: () => [],
    },
});

const container = ref(null);
const hasData = computed(() => props.series.some((s) => s.dataPoints.length > 0));

function buildTraces() {
    return props.series.map((s) => {
        const sorted = [...s.dataPoints].sort((a, b) => a.datum.localeCompare(b.datum));
        return {
            type: 'scatter',
            mode: 'lines+markers',
            name: s.bezeichnung,
            x: sorted.map((d) => String(d.datum).slice(0, 10)),
            y: sorted.map((d) => Math.round(d.auslastung * 10) / 10),
            hovertemplate: '<b>%{fullData.name}</b><br>%{x}<br>%{y:.1f}%<extra></extra>',
        };
    });
}

const layout = {
    yaxis: {
        title: { text: 'Auslastung (%)' },
        range: [0, 105],
        ticksuffix: '%',
    },
    xaxis: {
        type: 'date',
    },
    legend: {
        orientation: 'h',
        yanchor: 'top',
        y: -0.2,
        xanchor: 'left',
        x: 0,
    },
    margin: { t: 20, r: 10, b: 100, l: 60 },
    autosize: true,
};

const config = {
    responsive: true,
    displaylogo: false,
    modeBarButtonsToRemove: ['select2d', 'lasso2d', 'autoScale2d'],
    locale: 'de',
};

function render() {
    if (!container.value) return;
    if (!hasData.value) return;
    Plotly.react(container.value, buildTraces(), layout, config);
}

onMounted(render);
watch(() => props.series, render, { deep: true });
onBeforeUnmount(() => {
    if (container.value) Plotly.purge(container.value);
});
</script>

<template>
    <div v-if="hasData" ref="container" style="width: 100%; min-height: 420px" />
    <p v-else class="text-surface-400 text-sm italic">Noch keine Anwesenheitsdaten vorhanden.</p>
</template>
