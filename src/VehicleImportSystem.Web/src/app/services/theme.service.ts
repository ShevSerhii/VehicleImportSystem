import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  isDarkMode = signal<boolean>(false);

  constructor() {
    this.loadTheme();
  }

  toggleTheme() {
    this.isDarkMode.set(!this.isDarkMode());
    this.updateTheme(this.isDarkMode());
  }

  private loadTheme() {
    const savedTheme = localStorage.getItem('theme');
    const isDark = savedTheme === 'dark' || 
      (!savedTheme && window.matchMedia('(prefers-color-scheme: dark)').matches);
    
    this.isDarkMode.set(isDark);
    this.updateTheme(isDark);
  }

  private updateTheme(isDark: boolean) {
    const theme = isDark ? 'dark' : 'light';
    document.documentElement.setAttribute('data-theme', theme);
    document.body.style.colorScheme = theme;
    localStorage.setItem('theme', theme);
  }
}