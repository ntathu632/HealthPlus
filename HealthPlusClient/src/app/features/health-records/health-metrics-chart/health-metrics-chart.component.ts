import { Component, Input, OnChanges } from '@angular/core';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartData } from 'chart.js';
import {
  Chart, LineController, LineElement, PointElement,
  LinearScale, CategoryScale, Tooltip, Legend, Filler,
} from 'chart.js';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { HealthMetric } from '../../../models/health-record.models';

Chart.register(LineController, LineElement, PointElement, LinearScale, CategoryScale, Tooltip, Legend, Filler);

type ChartMode = 'bmi' | 'weight' | 'heartRate' | 'bloodPressure';

@Component({
    templateUrl: './health-metrics-chart.component.html',
    styleUrl: './health-metrics-chart.component.scss',
    selector: 'app-health-metrics-chart',
  standalone: true,
  imports: [BaseChartDirective, MatButtonToggleModule, FormsModule, DatePipe]
})
export class HealthMetricsChartComponent implements OnChanges {
  @Input() metrics: HealthMetric[] = [];

  selectedMode: ChartMode = 'bmi';

  chartData: ChartData<'line'> = { labels: [], datasets: [] };
  chartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        backgroundColor: 'white',
        titleColor: '#1A237E',
        bodyColor: '#546E7A',
        borderColor: '#E3F2FD',
        borderWidth: 1,
        padding: 10,
        boxPadding: 4,
      },
    },
    scales: {
      x: {
        grid: { color: '#F0F4F8' },
        ticks: { color: '#546E7A', font: { size: 11 } },
      },
      y: {
        grid: { color: '#F0F4F8' },
        ticks: { color: '#546E7A', font: { size: 11 } },
      },
    },
    elements: {
      line:  { tension: 0.4, borderWidth: 2 },
      point: { radius: 4, hoverRadius: 6 },
    },
  };

  private sorted: HealthMetric[] = [];

  ngOnChanges(): void {
    this.sorted = [...this.metrics].sort(
      (a, b) => new Date(a.measuredAt).getTime() - new Date(b.measuredAt).getTime()
    );
    this.autoSelectMode();
    this.updateChart();
  }

  private autoSelectMode(): void {
    if (this.hasBmi())           this.selectedMode = 'bmi';
    else if (this.hasWeight())   this.selectedMode = 'weight';
    else if (this.hasHeartRate()) this.selectedMode = 'heartRate';
    else if (this.hasBloodPressure()) this.selectedMode = 'bloodPressure';
  }

  updateChart(): void {
    const labels = this.sorted.map(m => {
      const d = new Date(m.measuredAt);
      return `${d.getDate()}/${d.getMonth() + 1}`;
    });

    switch (this.selectedMode) {
      case 'bmi':
        this.chartData = {
          labels,
          datasets: [{
            label: 'BMI',
            data: this.sorted.map(m => m.bmi ?? null),
            borderColor: '#1976D2',
            backgroundColor: 'rgba(25,118,210,0.08)',
            fill: true,
          }],
        };
        break;

      case 'weight':
        this.chartData = {
          labels,
          datasets: [{
            label: 'Cân nặng (kg)',
            data: this.sorted.map(m => m.weightKg ?? null),
            borderColor: '#00796B',
            backgroundColor: 'rgba(0,121,107,0.08)',
            fill: true,
          }],
        };
        break;

      case 'heartRate':
        this.chartData = {
          labels,
          datasets: [{
            label: 'Nhịp tim (bpm)',
            data: this.sorted.map(m => m.heartRate ?? null),
            borderColor: '#E91E63',
            backgroundColor: 'rgba(233,30,99,0.08)',
            fill: true,
          }],
        };
        break;

      case 'bloodPressure':
        this.chartData = {
          labels,
          datasets: [
            {
              label: 'Tâm thu (mmHg)',
              data: this.sorted.map(m => m.systolicBp ?? null),
              borderColor: '#C62828',
              backgroundColor: 'rgba(198,40,40,0.05)',
              fill: false,
            },
            {
              label: 'Tâm trương (mmHg)',
              data: this.sorted.map(m => m.diastolicBp ?? null),
              borderColor: '#2196F3',
              backgroundColor: 'rgba(33,150,243,0.05)',
              fill: false,
            },
          ],
        };
        (this.chartOptions!.plugins!.legend as any).display = true;
        break;
    }

    if (this.selectedMode !== 'bloodPressure') {
      (this.chartOptions!.plugins!.legend as any).display = false;
    }
  }

  hasBmi():           boolean { return this.sorted.some(m => m.bmi !== null && m.bmi !== undefined); }
  hasWeight():        boolean { return this.sorted.some(m => m.weightKg); }
  hasHeartRate():     boolean { return this.sorted.some(m => m.heartRate); }
  hasBloodPressure(): boolean { return this.sorted.some(m => m.systolicBp); }
}
