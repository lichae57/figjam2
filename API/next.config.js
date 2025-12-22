/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  images: {
    remotePatterns: [
      {
        protocol: 'https',
        hostname: 'interaktifkredi.com.tr',
        pathname: '/images/**',
      },
    ],
  },
}

module.exports = nextConfig

