import { Component, inject } from '@angular/core';
import { CardModule } from 'primeng/card';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CardModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  readonly auth = inject(AuthService);

  get nomeUtente(): string {
    const u = this.auth.currentUser();
    return u ? `${u.given_name}` : '';
  }
}
