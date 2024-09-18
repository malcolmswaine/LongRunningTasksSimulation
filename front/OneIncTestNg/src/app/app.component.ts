import { Component, OnDestroy, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SignalRService } from './services/signalr.service';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Subject, catchError, takeUntil, throwError } from 'rxjs';
import { environment } from './../environments/environment';
import { FormsModule } from '@angular/forms';

enum RunningStateEnum {
  Ready,
  Running,
  Complete,
  Cancelled,
  Error
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, FormsModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit, OnDestroy {

  // UI State
  error = '';
  stringToConvert = '';
  runningState = RunningStateEnum.Ready;
  runningStateEnum: typeof RunningStateEnum = RunningStateEnum;

  // This is the string data we get from the server
  serverStringData = '';

  // Shut down observables
  private ngUnsubscribe = new Subject<void>();

  // Keep track of the current job
  currentProcessingJobId: string = '';

  constructor(public signalRService: SignalRService, 
    private http: HttpClient
  ) {}


  ngOnInit() {

    // Get notification of task step complete on server
    this.signalRService.jobDataReceivedSubject
      .pipe(
        takeUntil(this.ngUnsubscribe)
       )
      .subscribe(message => {
        this.serverStringData += message;
    })

    // Job is complete
    this.signalRService.jobDataCompleteSubject
      .pipe(
        takeUntil(this.ngUnsubscribe)
       )
      .subscribe(message => {
        this.runningState = RunningStateEnum.Complete;
    })

    // Job was cancelled by user
    this.signalRService.jobCancelledSubject
      .pipe(
        takeUntil(this.ngUnsubscribe)
       )
      .subscribe(message => {
        this.runningState = RunningStateEnum.Cancelled;
    })

    // Job threw an exception
    this.signalRService.jobErrorSubject
    .pipe(
      takeUntil(this.ngUnsubscribe)
     )
    .subscribe(message => {
      this.error = message;
      this.runningState = RunningStateEnum.Error;
    })
  }


  // Start a new long running job on the server
  startNewJob() {

    console.log("Starting new job");

    // basic checks...
    if(this.stringToConvert.length === 0) return;

    // clear down any old data
    this.serverStringData = '';
    this.error = '';

    // Start the job on the server
    const params = new HttpParams().set('sigrConnId', this.signalRService.sigrConnId!);
    const payload = {stringToConvert: this.stringToConvert};
    this.http.post<string>(`${environment.WEBAPIHOST}/Jobs`, 
      payload, 
      {params}
    )
    .pipe(
      catchError((error: HttpErrorResponse) => {
          console.error(error);
          this.error = error.message;
          return throwError(() => error);
      }))
      .subscribe(jobId => {
        console.log("Job started, id: ", jobId);
        this.currentProcessingJobId = jobId;
        this.runningState = RunningStateEnum.Running;
      })
  }


  // Cancel long running job on server
  cancelLongRunningJob(jobId: string) {

    console.log("cancelling job");    

    const params = new HttpParams()
      .set('sigrConnId', this.signalRService.sigrConnId!);

    this.http.put(`${environment.WEBAPIHOST}/Jobs/${jobId}`, null, {params})
    .pipe(
      catchError((error: HttpErrorResponse) => {
          console.error(error);
          this.error = error.message;
          return throwError(() => error);
      }))
      .subscribe(result => {
        console.log("job cancelled", result);
        this.runningState = RunningStateEnum.Cancelled;
      })
  }


  // Reset the UI state
  reset() {
    this.serverStringData = '';
    this.runningState = RunningStateEnum.Ready;
    this.error = '';
    this.currentProcessingJobId = '';
  }


  ngOnDestroy() {
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }

}
