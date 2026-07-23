import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-validationmessage',
  imports: [],
  templateUrl: './validationmessage.html',
  styleUrl: './validationmessage.css',
})
export class Validationmessage {
  @Input() erromessage: string[] | undefined;
}
