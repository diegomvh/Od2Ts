import { Airline } from './airline.interface';
import { Injectable } from '@angular/core';
import { HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { ODataEntityService, ODataContext, ODataEntityRequest, ODataEntitySet } from 'angular-odata';

@Injectable()
export class AirlinesService extends ODataEntityService<Airline> {
  static set: string = 'Airlines';
  
  protected resolveEntityKey(entity: Partial<Airline>) {
    return entity.AirlineCode;
  }
  
  
}