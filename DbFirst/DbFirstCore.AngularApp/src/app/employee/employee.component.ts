import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Employee, EmployeeService } from './employee.service';

@Component({
  selector: 'app-employee',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './employee.component.html'
})
export class EmployeeComponent implements OnInit {
  employees: Employee[] = [];
  selected: Employee | null = null;
  idInput = '';
  createModel: Employee = { firstName: '', lastName: '', email: null } as Employee;
  updateModel: Employee = { firstName: '', lastName: '', email: null } as Employee;
  loading = false;
  message = '';
  createErrors: string[] = [];
  updateErrors: string[] = [];
  deleteError = '';

  constructor(private svc: EmployeeService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    // Do not automatically load all employees on component init.
    // Data will be fetched when the user clicks the GET button.
  }

  getAll() {
    this.loading = true;
    this.message = '';
    this.svc.getAll().subscribe({
      next: data => {
        this.employees = data;
        this.loading = false;
        // Ensure Angular updates the view immediately in case change detection
        // wasn't triggered by the HTTP observable for any reason.
        try { this.cdr.detectChanges(); } catch { /* ignore in tests */ }
      },
      error: err => { this.message = err?.message ?? 'Error'; this.loading = false; try { this.cdr.detectChanges(); } catch { } }
    });
  }

  getById() {
    const id = Number(this.idInput);
    if (!id) { this.message = 'Enter valid id'; return; }
    this.loading = true;
    this.svc.getById(id).subscribe({
      next: e => {
        this.selected = e;
        // populate updateModel so the update form includes the id and current values
        this.updateModel = { ...(e as Employee) } as Employee;
        this.loading = false;
        try { this.cdr.detectChanges(); } catch { }
      },
      error: err => { this.message = err?.error ?? err?.message ?? 'Error'; this.loading = false; try { this.cdr.detectChanges(); } catch { } }
    });
  }

  create() {
    // client-side validation
    this.createErrors = [];
    if (!this.createModel.firstName || !this.createModel.firstName.toString().trim()) this.createErrors.push('First name is required.');
    if (!this.createModel.lastName || !this.createModel.lastName.toString().trim()) this.createErrors.push('Last name is required.');
    if (this.createErrors.length) { this.message = 'Fix validation errors'; return; }

    this.loading = true;
    this.message = '';
    this.svc.create(this.createModel).subscribe({
      next: e => {
        // API may return empty text or a JSON body. Try to parse when possible.
        let id: any = null;
        if (typeof e === 'string') {
          try { const parsed = JSON.parse(e); id = parsed?.id; } catch { /* not JSON */ }
        } else {
          id = e?.id;
        }
        this.message = id ? ('Created id=' + id) : 'Created';
        this.createErrors = [];
        this.loading = false;
        try { this.cdr.detectChanges(); } catch { }
      },
      error: err => {
        // Prefer server-provided validation details when available
        const serverMsg = err?.error ?? err?.message ?? 'Error';
        this.message = serverMsg;
        this.loading = false;
        try { this.cdr.detectChanges(); } catch { }
      }
    });
  }

  update() {
    // client-side validation
    this.updateErrors = [];
    const id = Number(this.updateModel.id ?? this.idInput);
    if (!id || isNaN(id) || id <= 0) this.updateErrors.push('Valid Employee ID is required.');
    if (!this.updateModel.firstName || !this.updateModel.firstName.toString().trim()) this.updateErrors.push('First name is required.');
    if (!this.updateModel.lastName || !this.updateModel.lastName.toString().trim()) this.updateErrors.push('Last name is required.');
    if (this.updateErrors.length) { this.message = 'Fix validation errors'; return; }

    // ensure the model has the id set so backend validation passes
    this.updateModel.id = id;
    this.loading = true;
    this.message = '';
    this.svc.update(id, this.updateModel).subscribe({
      next: res => { this.message = typeof res === 'string' && res ? res : 'Updated'; this.updateErrors = []; this.loading = false; try { this.cdr.detectChanges(); } catch { } },
      error: err => { this.message = err?.error ?? err?.message ?? 'Error'; this.loading = false; try { this.cdr.detectChanges(); } catch { } }
    });
  }

  delete() {
    const id = Number(this.idInput);
    this.deleteError = '';
    if (!id || isNaN(id) || id <= 0) { this.deleteError = 'Enter valid id to delete.'; this.message = 'Fix validation errors'; return; }
    if (!confirm('Delete id=' + id + '?')) return;
    this.loading = true;
    this.message = '';
    this.svc.delete(id).subscribe({
      next: res => { this.message = typeof res === 'string' && res ? res : 'Deleted'; this.loading = false; try { this.cdr.detectChanges(); } catch { } },
      error: err => { this.message = err?.error ?? err?.message ?? 'Error'; this.loading = false; try { this.cdr.detectChanges(); } catch { } }
    });
  }
}
