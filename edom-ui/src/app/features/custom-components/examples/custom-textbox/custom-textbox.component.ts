import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { CustomTextboxInputComponent } from '../../components/custom-textbox/custom-textbox.component';

@Component({
  selector: 'app-custom-textbox',
  standalone: true,
  imports: [FormsModule, CardModule, CustomTextboxInputComponent],
  templateUrl: './custom-textbox.component.html',
  styleUrl: './custom-textbox.component.scss'
})
export class CustomTextboxComponent {
  normalValue = '';
  uppercaseValue = '';
}
