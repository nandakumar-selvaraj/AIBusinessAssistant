import { Component, ElementRef, HostListener, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

type OpenMenu = 'notifications' | 'profile' | null;

interface Notice {
  initials: string;
  time: string;
  title: string;
  meta: string;
}

interface MenuLink {
  label: string;
  icon: string;
}

@Component({
  selector: 'app-header',
  imports: [RouterLink],
  templateUrl: './header.html',
  styleUrl: './header.scss',
})
export class Header {
  private readonly host = inject(ElementRef<HTMLElement>);

  public readonly openMenu = signal<OpenMenu>(null);

  protected readonly user = { name: 'Stebin Ben', role: 'UI/UX Designer', initials: 'SB' };

  protected readonly notices: Notice[] = [
    { initials: 'CD', time: '3:00 AM', title: "It's Cristina Danny's birthday today.", meta: '2 min ago' },
    { initials: 'AB', time: '6:00 PM', title: 'Aida Burg commented on your post.', meta: '5 August' },
    { initials: '!', time: '2:45 PM', title: 'There was a failure in your setup.', meta: '7 hours ago' },
    { initials: 'CD', time: '9:10 PM', title: 'Cristina Danny invited you to a meeting.', meta: 'Daily scrum meeting time' },
  ];

  protected readonly profileLinks: MenuLink[] = [
    { label: 'View profile', icon: 'bi-person' },
    { label: 'Account settings', icon: 'bi-gear' },
    { label: 'Billing', icon: 'bi-wallet2' },
    { label: 'Log out', icon: 'bi-box-arrow-right' },
  ];

  protected toggle(menu: Exclude<OpenMenu, null>): void {
    this.openMenu.update((current) => (current === menu ? null : menu));
  }

  @HostListener('document:click', ['$event'])
  protected onDocumentClick(event: MouseEvent): void {
    if (!this.host.nativeElement.contains(event.target as Node)) this.openMenu.set(null);
  }

  @HostListener('document:keydown.escape')
  protected onEscape(): void {
    this.openMenu.set(null);
  }
}
