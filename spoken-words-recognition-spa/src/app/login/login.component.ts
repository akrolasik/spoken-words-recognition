import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { SessionStorageService } from 'ngx-webstorage';
import { Guid } from 'guid-typescript';
import * as jwt_decode from 'jwt-decode';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  tenantId = 'd71f40ab-f3da-4c3c-bb08-746a60338321';
  clientId = '2866b31c-7df3-4252-9239-cc347a42b507';
  idToken: string;

  constructor(private route: ActivatedRoute, private storage: SessionStorageService) {
    this.route.fragment.subscribe(fragment => {
      if (fragment == null) {
        const token = this.storage.retrieve('id_token');
        if (token != null) {
          this.idToken = jwt_decode(token);
        } else {
          this.GetIdToken();
        }
      } else if (fragment.startsWith('id_token=')) {
        const param = fragment.split('&')[0];
        const token = param.split('=')[1];
        this.idToken = jwt_decode(token);
        this.storage.store('id_token', token);
      }
    });
  }

  GetIdToken() {
    const url = `https://login.microsoftonline.com/${this.tenantId}/oauth2/v2.0/authorize?` +
      `client_id=${this.clientId}&` +
      `response_type=id_token&` +
      `scope=openid%20profile&` +
      `nonce=${Guid.create().toString()}&` +
      `redirect_uri=http%3A%2F%2Flocalhost%3A4200%2Flogin`;

    window.location.href = url;
  }

  ngOnInit() {
  }
}
