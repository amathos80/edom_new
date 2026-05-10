import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { AuthService } from '../../../core/services/auth.service';
import { CustomTextboxInputComponent } from "../../custom-components/components/custom-textbox/custom-textbox.component";
import { CustomPasswordInputComponent } from '../../custom-components/components/custom-password/custom-password.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, ButtonModule, CardModule, MessageModule, CustomTextboxInputComponent, CustomPasswordInputComponent],
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
      next: (response) => {
        if (response.mustChangePassword) {
          localStorage.setItem('must_change_password', 'true');
          this.router.navigate(['/forced-password-change']);
        } else {
          this.router.navigate(['/dashboard']);
        }
      },
      error: (err) => {
        this.errorMessage.set(err.status === 401 ? 'Credenziali non valide.' : 'Errore di connessione. Riprovare.');
        this.loading.set(false);
      }
    });
  }
}
