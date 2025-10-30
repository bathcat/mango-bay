import { describe, it, expect } from 'vitest';
import { z } from 'zod';
import { isSchemaNullable } from './schema-utils';

describe('isSchemaNullable', () => {
  describe('should return true for nullable schemas', () => {
    it('should detect direct nullable schemas', () => {
      const schema = z.string().nullable();
      expect(isSchemaNullable(schema)).toBe(true);
    });

    it('should detect optional schemas', () => {
      const schema = z.string().optional();
      expect(isSchemaNullable(schema)).toBe(true);
    });

    it('should detect union schemas with null', () => {
      const schema = z.union([z.string(), z.null()]);
      expect(isSchemaNullable(schema)).toBe(true);
    });

    it('should detect object schemas that are nullable', () => {
      const schema = z.object({ name: z.string() }).nullable();
      expect(isSchemaNullable(schema)).toBe(true);
    });

    it('should detect complex nullable schemas', () => {
      const ReviewSchema = z.object({
        id: z.guid(),
        rating: z.number().int().min(1).max(5),
        notes: z.string(),
      });
      const nullableReviewSchema = ReviewSchema.nullable();
      expect(isSchemaNullable(nullableReviewSchema)).toBe(true);
    });

    it('should detect optional nullable schemas', () => {
      const schema = z.string().nullable().optional();
      expect(isSchemaNullable(schema)).toBe(true);
    });

    it('should detect nested optional schemas', () => {
      const schema = z.string().optional();
      expect(isSchemaNullable(schema)).toBe(true);
    });
  });

  describe('should return false for non-nullable schemas', () => {
    it('should return false for basic string schema', () => {
      const schema = z.string();
      expect(isSchemaNullable(schema)).toBe(false);
    });

    it('should return false for object schema', () => {
      const schema = z.object({ name: z.string() });
      expect(isSchemaNullable(schema)).toBe(false);
    });

    it('should return false for array schema', () => {
      const schema = z.array(z.string());
      expect(isSchemaNullable(schema)).toBe(false);
    });

    it('should return false for enum schema', () => {
      const schema = z.enum(['A', 'B', 'C']);
      expect(isSchemaNullable(schema)).toBe(false);
    });

    it('should return false for literal schema', () => {
      const schema = z.literal('test');
      expect(isSchemaNullable(schema)).toBe(false);
    });
  });

});
