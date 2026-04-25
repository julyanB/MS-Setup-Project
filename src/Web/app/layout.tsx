import type { Metadata } from "next";
import { Plus_Jakarta_Sans, Sora } from "next/font/google";
import { Providers } from "./providers";
import { SiteHeader } from "@/components/SiteHeader";
import "./globals.css";

const jakarta = Plus_Jakarta_Sans({
  subsets: ["latin"],
  variable: "--font-jakarta",
  display: "swap",
});

const sora = Sora({
  subsets: ["latin"],
  variable: "--font-sora",
  display: "swap",
});

export const metadata: Metadata = {
  title: "Digi",
  description: "Digi — frontend for the gateway",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html
      lang="en"
      className={`${jakarta.variable} ${sora.variable}`}
      suppressHydrationWarning
    >
      <body suppressHydrationWarning>
        <div className="orb orb-1" aria-hidden />
        <div className="orb orb-2" aria-hidden />
        <div className="orb orb-3" aria-hidden />

        <Providers>
          <div className="relative z-10 min-h-screen">
            <SiteHeader />
            <main className="mx-auto max-w-7xl px-6 py-10">{children}</main>
          </div>
        </Providers>
      </body>
    </html>
  );
}
