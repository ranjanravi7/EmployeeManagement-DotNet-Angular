import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Employee {
  id?: number;
  firstName: string;
  lastName: string;
  email?: string | null;
  hireDate?: string;
  salary?: number;
  departmentId?: number | null;
  isActive?: boolean;
}

@Injectable({ providedIn: 'root' })
export class EmployeeService {
  // Match the actual ASP.NET Core API port used by the backend.
  private baseUrl = 'http://localhost:5202/api/Employees';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Employee[]> {
    return this.http.get<Employee[]>(this.baseUrl);
  }

  getById(id: number): Observable<Employee> {
    return this.http.get<Employee>(`${this.baseUrl}/${id}`);
  }

  // Some APIs return empty body on create/update/delete which causes
  // HttpClient JSON parse errors. Use responseType: 'text' to avoid parse errors
  // and let callers handle the returned text or JSON as needed.
  create(employee: Employee): Observable<any> {
    return this.http.post<any>(this.baseUrl, employee, { responseType: 'text' as 'json' });
  }

  update(id: number, employee: Employee): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${id}`, employee, { responseType: 'text' as 'json' });
  }

  delete(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/${id}`, { responseType: 'text' as 'json' });
  }
}
