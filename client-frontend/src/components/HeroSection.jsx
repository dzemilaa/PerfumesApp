import React from 'react'
import perfumeImg from '../assets/perfumes.jpg'

const HeroSection = () => {
  return (
    <div className="relative w-full h-[60vh] sm:h-screen" id='home'>
      <img
        src={perfumeImg}
        alt="Perfumes"
        className="w-full h-full object-cover"
      />
      <div className="absolute inset-0 bg-black/50"></div>
      <div className="absolute inset-0 flex flex-col items-center justify-center text-white text-center px-4">
        <h1 className="text-4xl md:text-6xl font-bold mb-4">
          Find Your Signature Scent
        </h1>
        <p className="text-lg md:text-xl mb-6">
          Luxury perfumes for every moment
        </p>

      </div>

    </div>
  )
}

export default HeroSection
