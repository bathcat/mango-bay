declare const __brand: unique symbol;
type Brand<B> = { [__brand]: B };
export type Branded<T, B> = T & Brand<B>;

export type BrandedObject<T extends object, B> = T & { readonly __brand: B };
