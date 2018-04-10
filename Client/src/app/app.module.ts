import {HttpModule} from '@angular/http';
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import { VideoRegService } from '../services/VideoReg.service';
import { NgConfigureModule, ConfigureOptions } from 'ng4-configure/ng4-configure'
import { MyOptions } from './MyOptions';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    HttpModule,
    NgConfigureModule.forRoot()
  ],
  providers: [ VideoRegService,
    { provide: ConfigureOptions, useClass: MyOptions }],
  bootstrap: [AppComponent]
})
export class AppModule { }
