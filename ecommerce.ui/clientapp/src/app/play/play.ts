import { Component, OnInit } from '@angular/core';
import { PlayService } from './play.service';

@Component({
  selector: 'app-play',
  imports: [],
  templateUrl: './play.html',
  styleUrl: './play.css',
})
export class Play implements OnInit {
  constructor(public playService: PlayService) { }

  ngOnInit(): void {
    this.playService.getPlay().subscribe({
      next: (response: any) => {
        if (response && response.statusCode === 200) {
          this.playService.$play.set(response);
        }
        return null;
      }
    });
  }
}
