# Build stage
FROM node:18-alpine AS build

WORKDIR /app

# Copy package files
COPY package.json ./

# Install dependencies with verbose logging
RUN npm install --verbose

# Copy source files
COPY . .

# List files to debug
RUN ls -la src/

# Build with verbose output
RUN npm run build -- --verbose

# List dist to verify build
RUN ls -la dist/

# Production stage
FROM nginx:alpine

# Copy built files
COPY --from=build /app/dist/phi-redaction-app /usr/share/nginx/html

# Simple nginx config
RUN echo 'server { \
    listen 80; \
    location / { \
        root /usr/share/nginx/html; \
        index index.html; \
        try_files $uri $uri/ /index.html; \
    } \
    location /health { \
        return 200 "ok"; \
        add_header Content-Type text/plain; \
    } \
}' > /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
