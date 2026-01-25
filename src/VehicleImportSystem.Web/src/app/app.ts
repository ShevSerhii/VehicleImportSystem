import { Component } from '@angular/core';
import { Calculator } from './components/calculator/calculator';
import { Header } from './components/header/header';
import { History } from './components/history/history';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [Header, Calculator, History],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  title = 'VehicleImportSystem.Web';
}