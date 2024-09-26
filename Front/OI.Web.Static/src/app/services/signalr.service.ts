import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { HubConnectionState } from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from './../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  
  private hubConnection: signalR.HubConnection;
  public connectedToServer: boolean = false;

  // Get notification of task step complete on server
  jobDataReceivedSubject = new Subject<string>();

  // Get notification of task step complete on server
  jobStartedSubject = new Subject<string>();

  // Job is complete
  jobDataCompleteSubject = new Subject();

  // Job was cancelled by user
  jobCancelledSubject = new Subject();

  // Job threw an exception
  jobErrorSubject = new Subject<string>();

  // Job threw an exception
  serverReconnectingSubject = new Subject();
  
  // THe connection id to the SignalR Server
  sigrConnId?: string;

  constructor() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.SIGNALRHOST + '/jobshub')
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: _ => {
          return 3000;
        }})
      .build();
    

    this.hubConnection.on('job-requested', (message) => {
      console.log(`job-requested": ${message}`);
      //this.jobErrorSubject.next(message);
    });

    this.hubConnection.on('job-started', (message) => {
      console.log(`job-started": ${message}`);
      this.jobStartedSubject.next(message);
    });


    this.hubConnection.on('job-processing-step', (message) => {
      console.log(`job-processing-step: ${message}`);
      this.jobDataReceivedSubject.next(message);
    });

    this.hubConnection.on('job-complete', (message) => {     
      console.log(`job-complete: ${message}`); 
      this.jobDataCompleteSubject.next({});
    });

    this.hubConnection.on('job-cancelled', (message) => {  
      console.log(`job-cancelled}`);    
      this.jobCancelledSubject.next({});
    });

    this.hubConnection.on('job-error', (message) => {
      console.error(`job-error": ${message}`);
      this.jobErrorSubject.next(message);
    });

    this.hubConnection.onclose((e) => {
      debugger;
      this.connectedToServer = false;
      this.sigrConnId = '';
      console.log(`signalr connection closed`);
    });

    this.hubConnection.onreconnecting((e) => {
      this.sigrConnId = '';
      this.connectedToServer = false;
      this.serverReconnectingSubject.next({});
      console.log('signalr connection dropped - reconnecting');
    });

    this.hubConnection.onreconnected(() => {
      this.connectedToServer = true;
      this.sigrConnId = this.hubConnection.connectionId!;
      console.log(`signalr connection restored`);
    });

    this.startConnection();
  }

  // Make a connection to the server
  startConnection() {
    this.hubConnection.start()
    .catch(err => {
      console.error(err);
      // If we can't connect, keep trying
      setTimeout(() => this.startConnection(), 3000);
    })
    .then(
      () => {
        // We're connected
        const hubConnectionState = this.hubConnection.state;
        if(hubConnectionState === HubConnectionState.Connected) {
          this.connectedToServer = true;
          this.sigrConnId = this.hubConnection.connectionId as string;
        }          
      });
  }
}