import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subscription } from 'rxjs';
import { HistoryRecord } from '../../models/calculation.models';
import { VehicleApiService } from '../../services/vehicle-api';

@Component({
  selector: 'app-history',
  standalone: true,
  imports: [
    CommonModule, 
    MatTableModule, 
    MatCardModule, 
    MatIconModule, 
    MatButtonModule, 
    MatProgressSpinnerModule
  ],
  templateUrl: './history.html',
  styleUrl: './history.scss',
})
export class History implements OnInit, OnDestroy {
  displayedColumns: string[] = ['date', 'car', 'price', 'total', 'profit', 'actions'];
  dataSource: HistoryRecord[] = [];
  isLoading = false;
  error: string | null = null;
  
  private refreshSub?: Subscription;

  constructor(private api: VehicleApiService) {}

  ngOnInit(): void {
    this.loadHistory();

    this.refreshSub = this.api.refreshHistory$.subscribe(() => {
      this.loadHistory();
    });
  }

  ngOnDestroy(): void {
    this.refreshSub?.unsubscribe();
  }

  loadHistory(): void {
    this.isLoading = true;
    this.error = null;

    this.api.getHistory().subscribe({
      next: (data) => {
        this.dataSource = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Не вдалося завантажити історію';
        this.isLoading = false;
        console.error(err);
      },
    });
  }

  deleteRecord(id: number): void {
    if (confirm('Видалити цей розрахунок?')) {
      this.api.deleteRecord(id).subscribe({
        next: () => {
          this.dataSource = this.dataSource.filter((item) => item.id !== id);
        },
        error: (err) => alert('Помилка при видаленні')
      });
    }
  }

  clearAll(): void {
    if (confirm('Очистити всю історію розрахунків?')) {
      this.isLoading = true;
      
      this.api.clearHistory().subscribe({
        next: () => {
          this.dataSource = [];
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
          alert('Не вдалося очистити історію');
        }
      });
    }
  }
}