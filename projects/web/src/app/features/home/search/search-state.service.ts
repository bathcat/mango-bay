import { Injectable, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { map, switchMap } from 'rxjs';
import { toLoadable } from '@app/shared/loadable';
import { of } from 'rxjs';

interface SearchResult {
  title: string;
  url: string;
  description: string;
}

const MOCK_RESULTS: SearchResult[] = [
  {
    title: 'Cargo Delivery to Remote Islands',
    url: 'https://mangobay.example/deliveries/remote-islands',
    description: 'Our seaplane cargo delivery service connects remote island communities with essential supplies and goods. Fast, reliable, and weather-dependent service available year-round.'
  },
  {
    title: 'Pilot Certification Requirements',
    url: 'https://mangobay.example/pilots/certification',
    description: 'Information about pilot certification requirements for seaplane cargo operations. Includes licensing, training, and safety protocols for cargo pilots.'
  },
  {
    title: 'Cargo Weight Limits and Restrictions',
    url: 'https://mangobay.example/cargo/limits',
    description: 'Detailed cargo weight limits and restrictions for seaplane deliveries. Learn about maximum payload, dimensional restrictions, and special handling requirements.'
  },
  {
    title: 'Island Site Locations',
    url: 'https://mangobay.example/sites/locations',
    description: 'Browse all cargo delivery site locations across the island network. Find pickup and dropoff sites with coordinates, facilities, and access information.'
  },
  {
    title: 'Emergency Cargo Delivery Services',
    url: 'https://mangobay.example/services/emergency',
    description: 'Need urgent cargo delivery? Our emergency seaplane cargo service provides rapid response for time-sensitive deliveries to remote locations.'
  }
];

@Injectable()
export class SearchStateService {
  private route = inject(ActivatedRoute);

  searchTerm$ = this.route.queryParamMap.pipe(
    map(params => params.get('q') || '')
  );

  results$ = this.searchTerm$.pipe(
    switchMap(searchTerm => {
      if (!searchTerm.trim()) {
        return toLoadable(of([]));
      }

      const filteredResults = MOCK_RESULTS.filter(result => 
        result.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
        result.description.toLowerCase().includes(searchTerm.toLowerCase())
      );

      return toLoadable(of(filteredResults));
    })
  );
}

