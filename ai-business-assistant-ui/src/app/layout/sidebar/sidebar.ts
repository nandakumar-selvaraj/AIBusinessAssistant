import { Component, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

interface NavItem {
  label: string;
  path: string;
  icon: string;
}

const STORAGE_KEY = 'businessai.sidebar.collapsed';

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss',
})
export class Sidebar {
  protected readonly collapsed = signal(this.initialState());

  protected readonly mainMenu: NavItem[] = [
    { label: 'Dashboard', path: '/dashboard', icon: 'bi-grid' },
    { label: 'Customers', path: '/customers', icon: 'bi-people' },
    { label: 'Products', path: '/products', icon: 'bi-box' },
    { label: 'Orders', path: '/orders', icon: 'bi-cart' },
  ];

  protected readonly secondaryMenu: NavItem[] = [
    { label: 'AI Assistant', path: '/ai-assistant', icon: 'bi-robot' },
    { label: 'Settings', path: '/settings', icon: 'bi-gear' },
  ];

  protected toggle(): void {
    const next = !this.collapsed();
    this.collapsed.set(next);
    try {
      localStorage.setItem(STORAGE_KEY, next ? '1' : '0');
    } catch {
      // storage unavailable; the state just won't persist
    }
  }

  private initialState(): boolean {
    try {
      const saved = localStorage.getItem(STORAGE_KEY);
      if (saved !== null) return saved === '1';
    } catch {
      // ignore
    }
    return typeof window !== 'undefined' && window.innerWidth < 768;
  }
}
