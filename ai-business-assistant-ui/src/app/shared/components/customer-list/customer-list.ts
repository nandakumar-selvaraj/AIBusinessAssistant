import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Customer } from '../../../core/models/customer.model';

@Component({
  selector: 'app-customer-list',
  templateUrl: './customer-list.html',
  styleUrl: './customer-list.scss',
})
export class CustomerList {
  @Input({ required: true }) customers: Customer[] = [];
  @Input() showDetails = false;
  @Input() showActions = false;
  @Output() editCustomer = new EventEmitter<Customer>();
  @Output() deleteCustomer = new EventEmitter<Customer>();
}
