import { Photo } from './photo.interface';
import { Injectable } from '@angular/core';
import { HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { ODataEntityService, ODataContext, ODataEntityRequest, ODataEntitySet } from 'angular-odata';

@Injectable()
export class PhotosService extends ODataEntityService<Photo> {
  static set: string = 'Photos';
  
  protected resolveEntityKey(entity: Partial<Photo>) {
    return entity.Id;
  }
  
  
}