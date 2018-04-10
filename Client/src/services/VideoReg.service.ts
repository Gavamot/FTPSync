import {VideoReg} from '../class/VideoReg';
import { Injectable } from '@angular/core';
import { Http } from '@angular/http';

@Injectable()
export class VideoRegService {

  constructor(private http: Http){}

  private readonly controller : string = "locla/VideoReg/";

  getAll(): VideoReg[] {
      this.http.get("");
      return null;
  }
}
