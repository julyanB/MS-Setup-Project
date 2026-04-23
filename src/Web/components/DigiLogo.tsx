type DigiLogoProps = {
  size?: number;
  className?: string;
  title?: string;
};

export function DigiLogo({
  size = 32,
  className,
  title = "Digi",
}: DigiLogoProps) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 32 32"
      role="img"
      aria-label={title}
      className={className}
      xmlns="http://www.w3.org/2000/svg"
    >
      <defs>
        <linearGradient id="digi-bg" x1="0" y1="0" x2="1" y2="1">
          <stop offset="0%" stopColor="#E040FB" />
          <stop offset="55%" stopColor="#7C4DFF" />
          <stop offset="100%" stopColor="#448AFF" />
        </linearGradient>
        <linearGradient id="digi-mark" x1="0" y1="0" x2="1" y2="1">
          <stop offset="0%" stopColor="#ffffff" stopOpacity="1" />
          <stop offset="100%" stopColor="#ffffff" stopOpacity="0.85" />
        </linearGradient>
      </defs>

      <rect x="0" y="0" width="32" height="32" rx="9" fill="url(#digi-bg)" />

      <path
        d="M9 7.5h8.2c5.1 0 8.3 3.3 8.3 8.5s-3.2 8.5-8.3 8.5H9V7.5Zm4.4 3.7v9.6h3.5c2.9 0 4.5-1.8 4.5-4.8s-1.6-4.8-4.5-4.8h-3.5Z"
        fill="url(#digi-mark)"
      />

      <circle cx="24.2" cy="9" r="1.6" fill="#FF4081" />
    </svg>
  );
}
