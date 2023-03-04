import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Observable, Subscription } from 'rxjs';
import { AccountService } from 'src/app/account/account.service';
import { BasketService } from 'src/app/basket/basket.service';
import { IProduct } from 'src/app/shared/models/product';
import { ProductReview } from 'src/app/shared/models/review';
import { IUser } from 'src/app/shared/models/user';
import { BreadcrumbService } from 'xng-breadcrumb';
import { ShopService } from '../shop.service';

@Component({
  selector: 'app-product-details',
  templateUrl: './product-details.component.html',
  styleUrls: ['./product-details.component.scss']
})
export class ProductDetailsComponent implements OnInit {
  product: IProduct;
  quantity = 1;
  currentUser$: Observable<IUser>;
  ratings: Observable<any>;
  reviewFormGroup: FormGroup;
  selectedProductId: string;
  reviews: ProductReview[] = [];
  reviewForm: FormGroup;
  returnUrl: string;
  currentUserSubscription: Subscription;
  user: string;

  constructor(private shopService: ShopService, private activatedRoute: ActivatedRoute,
    private bcService: BreadcrumbService, private basketService: BasketService, private fb: FormBuilder, private accountService: AccountService) {
    this.bcService.set('@productDetails', ' ')
  }

  ngOnInit(): void {
    this.loadProduct();
    this.getReviews();
    this.createReviewForm();
    this.currentUser$ = this.accountService.currentUser$;

    this.currentUserSubscription = this.currentUser$.subscribe(currentUser => {
      if (currentUser) {
        console.log(currentUser.displayName);
        this.user=currentUser.displayName;
        // Do something with the displayName property here
      }
    });

  }

  createReviewForm() {
    this.reviewForm = new FormGroup({
      name:  new FormControl({disabled: true}),
      comment: new FormControl('')
    })
  }

  onSubmit() {
    this.reviewForm.controls['name'].setValue(this.user);
    console.log(this.reviewForm);
    this.shopService.postReview(+this.activatedRoute.snapshot.paramMap.get('id'),this.reviewForm.value).subscribe(() => {
      this.getReviews();
      this.reviewForm.reset();
    }, error => {
      console.log(error);
    })
  }

  loadProduct() {
    this.shopService.getProduct(+this.activatedRoute.snapshot.paramMap.get('id')).subscribe(product => {
      this.product = product;
      this.bcService.set('@productDetails', product.name);
    }, error => {
      console.log(error);
    })
  }

  addItemToBasket() {
    this.basketService.addItemToBasket(this.product, this.quantity);
  }

  incrementQuantity() {
    this.quantity++;
  }

  decrementQuantity() {
    if (this.quantity > 1) {
      this.quantity--;
    }
  }

  getReviews() {
    this.shopService.getProductReviews(+this.activatedRoute.snapshot.paramMap.get('id')).subscribe(response => {
      this.reviews = response;
    }, error => {
      console.log(error);
    })
}

initializeForm() {
    this.reviewFormGroup = this.fb.group({
         username: ['', Validators.required],
         summary: ['', Validators.required],
         review: ['', Validators.required],
         productId: [this.selectedProductId]
    });
}

// submitReview() {
//     let body: ProductReview = this.reviewFormGroup.value;
//     this.shopService.postReview(body);
//     this.initializeForm();
// }

rateProduct(val) {

    // this.shopService.postRating({
    //      productId: this.selectedProductId,
    //      ratingValue: val
    // });

    // this.shopService.getProductRating(this.selectedProductId).subscribe((retVal) => {
    //     const ratings = retVal.map(v => v.ratingValue);
    //      let avRating = (ratings.length ? ratings.reduce((total, val) => total + val) / retVal.length : 0);

    //     //  this.productService.setProductRating(this.selectedProductId,avRating.toFixed(1));
    // });


}
}
