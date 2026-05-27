import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class Theme {
  isDarkTheme = signal<boolean>(false);

  constructor(){
    this.initTheme();
  }

  private initTheme() {
    const storedTheme = localStorage.getItem('user-theme');
    
    if (storedTheme) {
      this.setDarkTheme(storedTheme === 'dark');
    } else {
      const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
      this.setDarkTheme(prefersDark);
    }
  }

  toggleTheme() {
    this.setDarkTheme(!this.isDarkTheme());
  }

  private setDarkTheme(isDark: boolean) {
    this.isDarkTheme.set(isDark);
    localStorage.setItem('user-theme', isDark ? 'dark' : 'light');
    
    if (isDark) {
      document.documentElement.classList.add('dark-theme');
    } else {
      document.documentElement.classList.remove('dark-theme');
    }
  }
}
