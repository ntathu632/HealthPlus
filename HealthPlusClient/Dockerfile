FROM node:20-alpine AS build
WORKDIR /src

COPY package.json package-lock.json ./
# --legacy-peer-deps: package.json has a pre-existing peer-dep mismatch
# (@angular/animations ^22.0.0 vs @angular/core ^21.2.0), unrelated to this Dockerfile.
RUN npm ci --legacy-peer-deps

COPY . .
RUN npm run build -- --configuration production

FROM nginx:1.27-alpine AS final
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /src/dist/HealthPlusClient/browser /usr/share/nginx/html

EXPOSE 80
