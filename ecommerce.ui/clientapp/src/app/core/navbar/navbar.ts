import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AccountService } from '../../account/account.service';

@Component({
  selector: 'app-navbar',
  imports: [
    RouterLinkActive,
    RouterLink,
    CommonModule
  ],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {
  collapsed = false;

  constructor(public accountService: AccountService){

  }

  toggleCollapsed() {
    this.collapsed = !this.collapsed;
  }

  logout()
  {
    
  }
}
