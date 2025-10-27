import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'mbc-home',
  standalone: true,
  imports: [RouterLink, MatButtonModule, MatCardModule],
  template: `
    <section class="hero">
      <div class="hero-content">
        <img
          src="assets/sea-plane.webp"
          alt="Mango Bay Cargo Seaplane"
          class="hero-image"
        />
        <div class="hero-text">
          <h1>Welcome to Mango Bay Cargo</h1>
          <p class="tagline">
            The most trusted name in seaplane cargo delivery across the tropical islands
            and beyond
          </p>
          <div class="hero-buttons">
            <button mat-raised-button color="primary" routerLink="/pilots">
              View Our Pilots
            </button>
            <button mat-raised-button color="accent" routerLink="/sites">
              Places We Serve
            </button>
          </div>
        </div>
      </div>
    </section>

    <section class="features">
      <mat-card>
        <mat-card-header>
          <mat-card-title>Fast & Reliable</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          Our experienced pilots deliver your cargo on time, every time. From
          small packages to full freight loads, we've got you covered.
        </mat-card-content>
      </mat-card>

      <mat-card>
        <mat-card-header>
          <mat-card-title>Expert Pilots</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          Each of our pilots brings years of experience navigating the skies and
          waterways. Your cargo is in the best hands.
        </mat-card-content>
      </mat-card>

      <mat-card>
        <mat-card-header>
          <mat-card-title>Competitive Rates</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          Quality service doesn't have to break the bank. We offer transparent
          pricing and flexible booking options for all your shipping needs.
        </mat-card-content>
      </mat-card>
    </section>
  `,
})
export class HomeComponent {}

