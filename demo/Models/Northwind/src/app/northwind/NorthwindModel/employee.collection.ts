import { Employee } from './employee.model';
import { Collection, ODataCollection } from 'angular-odata';

export class EmployeeCollection extends ODataCollection<Employee> {
  static type = 'NorthwindModel.EmployeeCollection';
  static modelType = 'NorthwindModel.Employee';
}