import { z } from 'zod';

/**
 * Determines if a Zod schema can accept null values.
 * This is used to determine if 404 HTTP responses should be treated as null
 * instead of throwing an error.
 */
export function isSchemaNullable<T>(schema: z.ZodSchema<T>): boolean {
  return schema.isOptional() || schema.isNullable() || schema instanceof z.ZodUnion;
}
