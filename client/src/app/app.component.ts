import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone:true,
  imports: [CommonModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit{
  title = 'The Dating App';
  users:any;

  constructor(private http: HttpClient){}


  ngOnInit(){
      this.getUsers();
  }

  getUsers()
  {
    this.http.get('http://localhost:5069/api/user').subscribe(resposnse => {
      this.users = resposnse;
    },error => {
      console.log(error);
    });
  }
}
