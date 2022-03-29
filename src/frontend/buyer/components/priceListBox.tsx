import React from 'react'
import { Box, HStack, Text } from "@chakra-ui/react"

import { EventSearchFullDetail, EventSearchResult } from '../contracts/backendApi'
import { PriceLevelProps } from '../contracts/props'

const PriceLevelBox = (props: PriceLevelProps) => {
    return (
        <HStack>
            <Box style={{backgroundColor: `#${props.colorHexRgb}`}} minH='20px' mt='5px' mb='5px'>
                <Text fontSize='1xl' fontWeight={800} p={4}>{props.name}</Text>
            </Box>
            <Text fontSize='1xl' fontWeight={600}>${parseInt(props.price.toString())}</Text>
        </HStack>
    )
}

export const PriceListBox = (props: { priceLevels: PriceLevelProps[] }) => {
    return (
        <>
            {props.priceLevels.map(p => <PriceLevelBox key={p.id} {...p} />)}
        </>
    )
} 
