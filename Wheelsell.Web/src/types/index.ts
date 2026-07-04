export interface UserProfile {
  id: number;
  username: string;
  email: string;
  name: string;
  surname: string;
  phone?: string | null;
  city: string;
  county: string;
  profilePhotoPath?: string | null;
  role: "User" | "Moderator" | "Admin";
  isEmailConfirmed: boolean;
  createdAt: string;
  averageRating: number;
  reviewsCount: number;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  user: UserProfile;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface Brand {
  id: number;
  name: string;
  logoPath?: string | null;
}

export interface CarModel {
  id: number;
  brandId: number;
  name: string;
}

export interface Currency {
  id: number;
  code: string;
  symbol: string;
  name: string;
}

export interface Feature {
  id: number;
  name: string;
}

export interface FeatureCategory {
  id: number;
  name: string;
  order: number;
  features: Feature[];
}

export interface AdvertSummary {
  id: number;
  title: string;
  brandName: string;
  modelName: string;
  year: number;
  mileage: number;
  fuelType: string;
  transmission: string;
  price: number;
  currencyCode: string;
  currencySymbol: string;
  city: string;
  county: string;
  status: "Active" | "Sold" | "OffSale";
  thumbnailPath?: string | null;
  createdAt: string;
  isFavorite: boolean;
}

export interface SaleHistoryItem {
  advertId: number;
  price: number;
  currencyCode: string;
  mileage: number;
  soldAt?: string | null;
  sellerUsername: string;
  buyerId?: number | null;
  buyerUsername?: string | null;
}

export interface AdvertDetail {
  id: number;
  title: string;
  description: string;
  brandId: number;
  brandName: string;
  carModelId: number;
  modelName: string;
  trim?: string | null;
  year: number;
  mileage: number;
  fuelType: string;
  transmission: string;
  bodyType: string;
  drivetrain: string;
  enginePowerHp: number;
  engineSizeLiters: number;
  color: string;
  ownersCount: number;
  condition: string;
  damageSeverity?: number | null;
  repairDescription?: string | null;
  price: number;
  currencyId: number;
  currencyCode: string;
  currencySymbol: string;
  sellerFullName: string;
  sellerCity: string;
  sellerEmail: string;
  sellerPhone: string;
  sellerId: number;
  sellerUsername: string;
  sellerProfilePhotoPath?: string | null;
  sellerAverageRating: number;
  status: "Active" | "Sold" | "OffSale";
  buyerId?: number | null;
  soldAt?: string | null;
  previousAdvertId?: number | null;
  createdAt: string;
  isFavorite: boolean;
  isOwner: boolean;
  imagePaths: string[];
  videoPaths: string[];
  features: string[];
  saleHistory: SaleHistoryItem[];
}

export interface CreateAdvertRequest {
  title: string;
  description: string;
  brandId: number;
  carModelId: number;
  trim?: string;
  year: number;
  mileage: number;
  fuelType: string;
  transmission: string;
  bodyType: string;
  drivetrain: string;
  enginePowerHp: number;
  engineSizeLiters: number;
  color: string;
  ownersCount: number;
  condition: string;
  damageSeverity?: number | null;
  repairDescription?: string;
  price: number;
  currencyId: number;
  sellerFullName?: string;
  sellerCity?: string;
  sellerEmail?: string;
  sellerPhone?: string;
  featureIds: number[];
}

export interface PurchaseHistoryItem {
  advertId: number;
  title: string;
  brandName: string;
  modelName: string;
  year: number;
  price: number;
  currencyCode: string;
  mileage: number;
  soldAt?: string | null;
  thumbnailPath?: string | null;
  sellerUsername: string;
}

export interface ConversationDto {
  id: number;
  advertId: number;
  advertTitle: string;
  advertThumbnailPath?: string | null;
  otherUserId: number;
  otherUsername: string;
  otherUserProfilePhotoPath?: string | null;
  lastMessage?: string | null;
  lastMessageAt?: string | null;
  unreadCount: number;
}

export interface MessageDto {
  id: number;
  conversationId: number;
  senderId: number;
  senderUsername: string;
  content: string;
  isRead: boolean;
  createdAt: string;
}

export interface ReviewDto {
  id: number;
  advertId: number;
  advertTitle: string;
  reviewerId: number;
  reviewerUsername: string;
  reviewerProfilePhotoPath?: string | null;
  revieweeId: number;
  rating: number;
  comment: string;
  createdAt: string;
}

export interface NotificationDto {
  id: number;
  type: string;
  message: string;
  isRead: boolean;
  relatedAdvertId?: number | null;
  relatedConversationId?: number | null;
  createdAt: string;
}

export interface AdminUserDto {
  id: number;
  username: string;
  email: string;
  name: string;
  surname: string;
  role: "User" | "Moderator" | "Admin";
  isEmailConfirmed: boolean;
  isBanned: boolean;
  banReason?: string | null;
  createdAt: string;
}

export interface BannedUserDto {
  id: number;
  username: string;
  email: string;
  banReason?: string | null;
  bannedAt?: string | null;
}

export interface BannedAdvertDto {
  id: number;
  title: string;
  sellerUsername: string;
  banReason?: string | null;
  bannedAt?: string | null;
}
