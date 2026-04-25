import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, ButtonModule, InputTextModule, PasswordModule, CardModule, MessageModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  loading = signal(false);
  errorMessage = signal<string | null>(null);

  form = this.fb.group({
    username: ['', [Validators.required]],
    password: ['', [Validators.required]]
  });

  onSubmit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.errorMessage.set(null);

    const { username, password } = this.form.value;
    this.auth.login({ username: username!, password: password! }).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: (err) => {
        this.errorMessage.set(err.status === 401 ? 'Credenziali non valide.' : 'Errore di connessione. Riprovare.');
        this.loading.set(false);
      }
    });
  }
}
