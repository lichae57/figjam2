import { z } from 'zod';

export const tcknSchema = z.string().length(11, 'TCKN 11 haneli olmalıdır').regex(/^\d+$/, 'TCKN sadece rakam içermelidir');

export const gsmSchema = z.string().min(10, 'GSM en az 10 haneli olmalıdır').max(11, 'GSM en fazla 11 haneli olmalıdır').regex(/^\d+$/, 'GSM sadece rakam içermelidir');

export const otpCodeSchema = z.string().length(6, 'OTP kodu 6 haneli olmalıdır').regex(/^\d+$/, 'OTP kodu sadece rakam içermelidir');

export const customerIdSchema = z.number().int().positive('Customer ID pozitif olmalıdır');

export const dealerIdSchema = z.number().int().positive('Dealer ID pozitif olmalıdır');

export const reportIdSchema = z.string().min(1, 'Report ID boş olamaz');

export const kvkkIdSchema = z.number().int().positive('KVKK ID pozitif olmalıdır');

