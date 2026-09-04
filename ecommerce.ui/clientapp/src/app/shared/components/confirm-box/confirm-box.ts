import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-confirm-box',
  imports: [],
  templateUrl: './confirm-box.html',
  styleUrl: './confirm-box.css',
})
export class ConfirmBox {
  @Input() message: string = "";
  result?: boolean;
  activeModel: boolean = false;
  constructor(){

  }
  yes()
  {
    this.activeModel = true;
  }
  no(){
    this.activeModel = false;
  }
} 
