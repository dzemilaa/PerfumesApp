import { useEffect, useState } from "react";

import p1 from "../assets/perfume1.jpg";
import p2 from "../assets/perfume2.jpg";
import p3 from "../assets/perfume3.jpg";
import p4 from "../assets/perfume4.jpg";

const images = [p1, p2, p3, p4];

export default function ImageSlider() {
  const [current, setCurrent] = useState(0);

  useEffect(() => {
    const interval = setInterval(() => {
      setCurrent((prev) => (prev + 1) % images.length);
    }, 3000);
    //(2 + 1) % 3 = 0 je da bi se vratio na pocetak kad dodje do 3 (ako imamo 3 slike)
    return () => clearInterval(interval);
  }, []);

  return (
    <div className="relative w-full overflow-hidden">
      <h1 className=" mt-6 mb-6 text-center text-4xl md:text-5xl font-bold">
        Most Popular
      </h1>

      <p className=" text-center text-gray-500">
        Our best-selling perfumes
      </p>

      <div
        className="flex transition-transform duration-700 ease-in-out"
        style={{ transform: `translateX(-${current * 100}%)` }}
      //current * 100% pomera slike ulevo za sirinu slike i postavlja se nova
      >
        {images.map((src, index) => (
          <div key={index} className="min-w-full flex justify-center">
            <img
              src={src}
              alt={`Perfume ${index + 1}`}
              className="w-full h-125 object-contain bg-white"
            />
          </div>
        ))}
      </div>

      <div className="absolute bottom-4 left-1/2 -translate-x-1/2 flex gap-2">
        {images.map((_, i) => (
          <button
            key={i}
            onClick={() => setCurrent(i)}
            className={`w-3 h-3 rounded-full ${current === i ? "bg-black" : "bg-gray-300"
              }`}
          />
        ))}
      </div>
    </div>
  );
}
