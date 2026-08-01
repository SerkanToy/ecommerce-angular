import { Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs';
import { ApiResponse } from '../shared/models/apiRespose';
import { PlayModel } from '../shared/models/play/play_model';

@Injectable({
  providedIn: 'root',
})
export class PlayService {
  apiUrl = `${environment.apiUrl}`;
  $play = signal<ApiResponse<PlayModel> | null>(null);

  constructor(private http: HttpClient) {}

  getPlay() {
    return this.http.get<ApiResponse<PlayModel>>(`${this.apiUrl}play/get-all`).pipe(map((playData: ApiResponse<PlayModel>) => {
          this.setPlay(playData);
        }));

  }

  private setPlay(user: ApiResponse<PlayModel>) {
      this.$play.set(user);
    }
}
