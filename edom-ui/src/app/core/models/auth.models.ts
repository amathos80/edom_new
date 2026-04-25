export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresIn: number;
  username: string;
  fullName: string;
  roles: string[];
}

export interface JwtPayload {
  sub: string;
  unique_name: string;
  given_name: string;
  role: string | string[];
  exp: number;
  iss: string;
  aud: string;
}
