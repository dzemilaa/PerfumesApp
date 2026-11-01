import React from 'react'

const Footer = () => {
  return (
    <div
      id="contact"
      className="mt-5 flex flex-col items-center gap-5 bg-[#323232] px-[8vw] pb-5 pt-20 text-[#d9d9d9]"
    >
      <div className="grid w-full grid-cols-[2fr_1fr] gap-20 max-[750px]:flex max-[750px]:flex-col max-[750px]:gap-9">

        <div className="flex flex-col items-start gap-5">
          <h2 className="text-white text-xl font-semibold">
            Welcome to Bella Perfumes online shop
          </h2>

          <p>
            Discover a curated collection of luxury, niche, and everyday fragrances.
            Shop your favorite perfumes with ease and enjoy a seamless online experience
            from browsing to checkout.
          </p>

          <p>
            Follow us on social media for the latest arrivals, exclusive offers, and
            fragrance tips.
          </p>

          <p>
            Thank you for choosing Bella Perfumes. We’re passionate about bringing you
            unforgettable scents that match your style and personality.
          </p>


        </div>

        <div className="flex flex-col items-start gap-5">
          <h2 className="text-white text-xl font-semibold">
            GET IN TOUCH
          </h2>

          <p>📍 Address: Gradska, Novi Pazar</p>
          <p>📞 Phone: (+381) 62-3101-004</p>
          <p>📧 Email: info@bellaperfumes.com</p>
        </div>
      </div>

      <hr className="my-5 h-0.5 w-full border-none bg-gray-500" />

      <p className="text-sm max-[750px]:text-center">
        Copyright 2026 © Bella.com - All right Reserved.
      </p>
    </div>
  )
}

export default Footer
