import { Routes } from '@angular/router';

export const routes: Routes = [
    {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
    },
    {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard').then((m) => m.Dashboard),
    },
    { path: 'customers', loadComponent: () => import('./features/customers/customers').then((m) => m.Customers) },
    { path: 'products', loadComponent: () => import('./features/products/products').then((m) => m.Products) },
    { path: 'orders', loadComponent: () => import('./features/items/items').then((m) => m.Items) },
    { path: 'ai-assistant', loadComponent: () => import('./features/ai-assistant/ai-assistant').then((m) => m.AiAssistant) },
    { path: 'items', redirectTo: 'orders', pathMatch: 'full' },
];
