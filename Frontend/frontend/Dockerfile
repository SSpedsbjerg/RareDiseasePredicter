# Use the official Node.js 16 image as the base image
FROM node:latest AS build

# Set the working directory to /app
WORKDIR /app

# Copy the package.json and package-lock.json files to the working directory
COPY package*.json ./

# Install the dependencies
RUN npm install --force

# Copy the entire contents of the app to the working directory
COPY . .

# Build the app for production
RUN npm run build

# Use the official Nginx image as the base image for the production environment
FROM nginx:alpine

# Copy the React build files from the previous stage to the default Nginx public directory
COPY --from=build /app/build /usr/share/nginx/html

# Copy the custom Nginx configuration file to the container
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80 for incoming traffic
EXPOSE 80

# Start the Nginx service
CMD ["nginx", "-g", "daemon off;"]
